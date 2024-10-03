using Quingo.Shared.Entities;
using System.Collections.ObjectModel;

namespace Quingo.Application.State
{
    public class NodeLinkSearch(Node node, ReadOnlyCollection<Node> searchNodes)
    {
        private Node Node { get; } = node;

        private ReadOnlyCollection<Node> SearchNodes { get; } = searchNodes;

        public IEnumerable<Node> Search()
        {
            var resultFrom = Node.NodeLinks.Where(n => SearchNodes
                .FirstOrDefault(dn => dn.Id != Node.Id && dn.Id == n.NodeFromId) != null)
                .Select(x => x.NodeFrom);
            foreach (var item in resultFrom)
            {
                yield return item;
            }

            var resultTo = Node.NodeLinks.Where(n => SearchNodes
                .FirstOrDefault(dn => dn.Id != Node.Id && dn.Id == n.NodeToId) != null)
                .Select(x => x.NodeTo);
            foreach (var item in resultTo)
            {
                yield return item;
            }

            var indirectLinks = Node.Pack.IndirectLinks.Where(x => x.Steps.Count > 0 && x.IsLinkedNode(Node));
            foreach (var indirectLink in indirectLinks)
            {
                var resultIndirect = IndirectLinkSearch(indirectLink);
                foreach (var item in resultIndirect)
                {
                    yield return item;
                }
            }
        }

        private IEnumerable<Node> IndirectLinkSearch(IndirectLink link)
        {

            foreach (var nodeTo in SearchNodes.Where(x => x != Node && link.IsLinkedNode(x)))
            {
                var reverse = Node.Tags.Contains(link.TagTo) && link.Direction != NodeLinkDirection.Both;
                var step = reverse ? link.Steps.Last() : link.Steps.First();
                var stepSearch = IndirectLinkSearchStep(link, step, [Node], nodeTo, reverse);
                foreach (var result in stepSearch)
                {
                    yield return result;
                }
            }
        }

        private IEnumerable<Node> IndirectLinkSearchStep(IndirectLink link, IndirectLinkStep step, IEnumerable<Node> stepNodes, Node nodeTo, bool reverse)
        {
            var steps = link.Steps;
            var isFirstStep = reverse ? steps.Last() == step : steps.First() == step;
            var isLastStep = reverse ? steps.First() == step : steps.Last() == step;
            var idxShift = reverse ? -1 : 1;
            var nextStep = isLastStep ? null : steps[steps.IndexOf(step) + idxShift];

            if (isLastStep && (link.Direction != NodeLinkDirection.Both || reverse))
            {
                foreach (var stepNode in stepNodes)
                {
                    var result = reverse 
                        ? stepNode.NodeLinksTo.Select(x => x.NodeFrom).FirstOrDefault(x => x == nodeTo) 
                        : stepNode.NodeLinksFrom.Select(x => x.NodeTo).FirstOrDefault(x => x == nodeTo);
                    if (result != null)
                    {
                        yield return result;
                    }
                }
            }
            else if (isLastStep && link.Direction == NodeLinkDirection.Both && !reverse)
            {
                foreach (var stepNode in stepNodes)
                {
                    var nextStepNodes = stepNode.NodeLinksFrom.Select(x => x.NodeTo).Where(x => x != nodeTo && x.Tags.Contains(step.TagTo));
                    var nextResult = IndirectLinkSearchStep(link, step, nextStepNodes, nodeTo, true);
                    foreach (var result in nextResult)
                    {
                        yield return result;
                    }
                }
            }
            else
            {
                foreach (var stepNode in stepNodes)
                {
                    var nextStepNodes = reverse 
                        ? stepNode.NodeLinksTo.Select(x => x.NodeFrom).Where(x => x != nodeTo && x.Tags.Contains(step.TagFrom)) 
                        : stepNode.NodeLinksFrom.Select(x => x.NodeTo).Where(x => x != nodeTo && x.Tags.Contains(step.TagTo));
                    var nextResult = IndirectLinkSearchStep(link, nextStep!, nextStepNodes, nodeTo, reverse);
                    foreach (var result in nextResult)
                    {
                        yield return result;
                    }
                }
            }
        }
    }
}
