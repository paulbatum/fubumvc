using System;
using System.Linq.Expressions;
using FubuMVC.Core.Runtime;
using FubuMVC.Core.Util;
using FubuMVC.UI.Configuration;
using HtmlTags;
using Microsoft.Practices.ServiceLocation;

namespace FubuMVC.UI.Tags
{
    public interface ITagGenerator<T> where T : class
    {
        void SetProfile(string profileName);
        HtmlTag LabelFor(Expression<Func<T, object>> expression);
        HtmlTag InputFor(Expression<Func<T, object>> expression);
        HtmlTag DisplayFor(Expression<Func<T, object>> expression);
        ElementRequest GetRequest(Expression<Func<T, object>> expression);
        HtmlTag LabelFor(ElementRequest request);
        HtmlTag InputFor(ElementRequest request);
        HtmlTag DisplayFor(ElementRequest request);
    }

    public class TagGenerator<T> : ITagGenerator<T> where T : class
    {
        private readonly TagProfileLibrary _library;
        private readonly T _model;
        private readonly IElementNamingConvention _namingConvention;
        private readonly IServiceLocator _services;
        private readonly Stringifier _stringifier;
        private TagProfile _profile;

        public TagGenerator(TagProfileLibrary library, IElementNamingConvention namingConvention,
                            IServiceLocator services, IFubuRequest request, Stringifier stringifier)
        {
            _model = request.Get<T>();
            _library = library;
            _namingConvention = namingConvention;
            _services = services;
            _stringifier = stringifier;

            _profile = _library.DefaultProfile;
        }

        public void SetProfile(string profileName)
        {
            _profile = _library[profileName];
        }

        private HtmlTag buildTag(Expression<Func<T, object>> expression, TagFactory factory)
        {
            ElementRequest request = GetRequest(expression);

            return factory.Build(request);
        }

        public ElementRequest GetRequest(Expression<Func<T, object>> expression)
        {
            var request = new ElementRequest(_model, expression.ToAccessor(), _services, _stringifier);
            request.ElementId = _namingConvention.GetName(typeof (T), request.Accessor);
            return request;
        }

        public HtmlTag LabelFor(ElementRequest request)
        {
            return _profile.Label.Build(request);
        }

        public HtmlTag InputFor(ElementRequest request)
        {
            return _profile.Editor.Build(request);
        }

        public HtmlTag DisplayFor(ElementRequest request)
        {
            return _profile.Display.Build(request);
        }

        public HtmlTag LabelFor(Expression<Func<T, object>> expression)
        {
            return buildTag(expression, _profile.Label);
        }

        public HtmlTag InputFor(Expression<Func<T, object>> expression)
        {
            return buildTag(expression, _profile.Editor);
        }

        public HtmlTag DisplayFor(Expression<Func<T, object>> expression)
        {
            return buildTag(expression, _profile.Display);
        }
    }
}