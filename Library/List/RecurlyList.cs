﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using HttpRequestMethod = Recurly.Client.HttpRequestMethod;

namespace Recurly
{
    public abstract class RecurlyList<T> : IEnumerable<T>
    {
        protected List<T> Items;
        internal HttpRequestMethod Method;
        protected string BaseUrl;

        protected string StartUrl { get; set; }
        protected string NextUrl { get; set; }
        protected string PrevUrl { get; set; }

        public int Count
        {
            get { return Items.Count; }
        }

        private int _capacity = -1;
        public int Capacity
        {
            get { return _capacity < 0 ? Items.Count : _capacity; }
        }

        public abstract RecurlyList<T> Start { get; }
        public abstract RecurlyList<T> Next { get; }
        public abstract RecurlyList<T> Prev { get; }

        public bool HasStartPage()
        {
            return !StartUrl.IsNullOrEmpty();
        }

        public bool HasNextPage()
        {
            return !NextUrl.IsNullOrEmpty();
        }

        public bool HasPrevPage()
        {
            return !PrevUrl.IsNullOrEmpty();
        }

        internal RecurlyList()
        {
        }

        internal RecurlyList(HttpRequestMethod method, string url)
        {
            Method = method;
            BaseUrl = url;

            GetItems();
        }

        protected void GetItems()
        {
            Client.Instance.PerformRequest(Method,
                ApplyPaging(BaseUrl),
                ReadXmlList);
        }

        protected string ApplyPaging(string baseUrl)
        {
            var divider = baseUrl.Contains("?") ? "&" : "?";
            return baseUrl + divider + "per_page=" + Client.Instance.Settings.PageSize;
        }

        internal void ReadXmlList(XmlTextReader xmlReader, int records, string start, string next, string prev)
        {
            if (Items == null)
            {
                Items = records > 0 ? new List<T>(records) : new List<T>();
            }
            _capacity = records;
            StartUrl = start;
            NextUrl = next;
            PrevUrl = prev;
            ReadXml(xmlReader);
        }

        internal abstract void ReadXml(XmlTextReader reader);

        protected void Add(T item)
        {
            if (Items == null)
            {
                Items = new List<T>();
            }

            Items.Add(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public T this[int i]
        {
            get { return Items[i]; }
            set { throw new NotSupportedException("RecurlyLists are readonly!"); }
        }
    }

    public class RecurlyList
    {
        public static RecurlyList<T> Empty<T>()
        {
            return EmptyRecurlyList<T>.Instance;
        }
    }

    internal class EmptyRecurlyList<T>
    {
        private static volatile EmptyRecurlyListImpl<T> _instance;

        public static RecurlyList<T> Instance
        {
            get { return _instance ?? (_instance = new EmptyRecurlyListImpl<T>()); }
        } 
    }

    internal class EmptyRecurlyListImpl<T> : RecurlyList<T>
    {
        public EmptyRecurlyListImpl()
        {
            Items = new List<T>();
        }

        public override RecurlyList<T> Start
        {
            get { return new EmptyRecurlyListImpl<T>(); }
        }

        public override RecurlyList<T> Next
        {
            get { return new EmptyRecurlyListImpl<T>(); }
        }

        public override RecurlyList<T> Prev
        {
            get { return new EmptyRecurlyListImpl<T>(); }
        }

        internal override void ReadXml(XmlTextReader reader)
        {
            throw new NotSupportedException("Empty Recurly Lists are read only!");
        }
    }
}
