using System;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace BackOfficeTests.Framework
{
    public abstract class ControllersTestBase<TController>
    {
        [SetUp]
        public void InitBase()
        {
            Controller = InitController();
            Setup();
        }

        protected abstract TController InitController();

        protected TController Controller { get; private set; }

        protected virtual void Setup()
        {

        }

        protected static TRequest MakeRequest<TRequest>(Action<TRequest> fillAction = null) where TRequest : class, new()
        {
            var request = new TRequest();
            fillAction?.Invoke(request);
            return request;
        }

        protected static TModel GetViewModel<TModel>(ActionResult actionResult)
        {
            var model = ((ViewResult)actionResult).Model;
            var casted = (TModel)model;
            return casted;
        }
    }
}
