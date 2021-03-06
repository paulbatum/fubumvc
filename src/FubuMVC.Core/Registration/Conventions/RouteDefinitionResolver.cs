using System;
using System.Collections.Generic;
using System.Linq;
using FubuMVC.Core.Registration.Nodes;
using FubuMVC.Core.Registration.Routes;

namespace FubuMVC.Core.Registration.Conventions
{
    // TODO -- need a way to ignore routes
    public class RouteDefinitionResolver
    {
        private readonly UrlPolicy _defaultUrlPolicy;
        private readonly RouteInputPolicy _inputPolicy = new RouteInputPolicy();
        private readonly List<IUrlPolicy> _policies = new List<IUrlPolicy>();
        private readonly RouteConstraintPolicy _constraintPolicy = new RouteConstraintPolicy();

        public RouteDefinitionResolver()
        {
            _defaultUrlPolicy = new UrlPolicy(call => true, _inputPolicy);
            _defaultUrlPolicy.IgnoreClassSuffix("controller");

            _inputPolicy.PropertyFilters.Includes +=
                prop => prop.InputProperty.HasCustomAttribute<RouteInputAttribute>();
            _inputPolicy.PropertyFilters.Includes +=
                prop => prop.InputProperty.HasCustomAttribute<QueryStringAttribute>();
            _inputPolicy.PropertyAlterations.Register(prop => prop.HasCustomAttribute<QueryStringAttribute>(),
                                                      (route, prop) => route.AddQueryInput(prop));

            _policies.Add(new UrlPatternAttributePolicy());
        }

        public RouteInputPolicy InputPolicy { get { return _inputPolicy; } }
        public UrlPolicy DefaultUrlPolicy { get { return _defaultUrlPolicy; } }
        public RouteConstraintPolicy ConstraintPolicy { get { return _constraintPolicy; } }

        public void Apply(BehaviorGraph graph, BehaviorChain chain)
        {
            ActionCall call = chain.Calls.FirstOrDefault();
            if (call == null) return;

            IUrlPolicy policy = _policies.FirstOrDefault(x => x.Matches(call)) ?? _defaultUrlPolicy;
            IRouteDefinition route = policy.Build(call);
            _constraintPolicy.Apply(call, route);
            graph.RegisterRoute(chain, route);
        }

        public void ApplyToAll(BehaviorGraph graph)
        {
            graph.Behaviors.ToList().Each(chain => Apply(graph, chain));
        }

        public void RegisterUrlPolicy(IUrlPolicy policy)
        {
            _policies.Add(policy);
        }

        public void RegisterRouteInputPolicy(Func<ActionCall, bool> where, Action<IRouteDefinition, ActionCall> action)
        {
            _inputPolicy.InputBuilders.Register(where, action);
        }
    }
}