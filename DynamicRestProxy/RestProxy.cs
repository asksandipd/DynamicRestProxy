﻿using System;
using System.Dynamic;
using System.Diagnostics;

using RestSharp;

namespace DynamicRestProxy
{
    public class RestProxy : DynamicObject
    {
        private RestClient _client;
        private RestProxy _parent;
        private string _name;

        internal char KeywordEscapeCharacter { get; private set; }

        public RestProxy(RestClient client, char keywordEscapeCharacter = '_')
            : this(client, null, "", keywordEscapeCharacter)
        {
        }

        internal RestProxy(RestClient client, RestProxy parent, string name, char keywordEscapeCharacter)
        {
            Debug.Assert(client != null);
            Debug.Assert(parent != null);

            _client = client;
            _parent = parent;
            _name = name;
            KeywordEscapeCharacter = keywordEscapeCharacter;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            // 'segment' is a special escape indicator to support url segments that are not valid C# identifiers
            // example: service.bills.mn.segment("2013s1").segment("SF 1").get()
            if (binder.Name == "segment")
            {
                if (args == null || args.Length != 1)
                    throw new InvalidOperationException("The escape sequence 'segment' must have exactly 1 unnamed parameter");

                result = new RestProxy(_client, this, args[0].ToString(), KeywordEscapeCharacter);
            }
            else
            {
                var builder = new RequestBuilder(this);
                var request = builder.BuildRequest(binder, args);
                var invocation = new RestInvocation(binder.Name);
                result = invocation.InvokeAsync(_client, request);
            }

            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            // this gets invoked when a dynamic property is accessed
            // example: proxy.locations will invoke here with a binder named location
            // each dynamic property is treated as a url segment
            result = new RestProxy(_client, this, binder.Name, KeywordEscapeCharacter);

            return true;
        }

        internal int Index
        {
            get
            {
                return _parent != null ? _parent.Index + 1 : -1; // the root is the main url - does not represent a url segment
            }
        }

        internal void AddSegment(RestRequest request)
        {
            if (_parent != null && _parent.Index != -1) // don't add a segemnt for the root element
            {
                _parent.AddSegment(request);
            }

            request.AddUrlSegment(Index.ToString(), _name.TrimStart(KeywordEscapeCharacter));
        }        
    }
}
