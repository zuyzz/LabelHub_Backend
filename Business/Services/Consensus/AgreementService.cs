namespace DataLabelProject.Business.Services.Consensus;

public interface IAgreementService
{
	double CalculateOverallScore(List<BoxCluster> clusters, int totalAnnotators);
	List<ConsensusBboxDto> BuildConsensusBboxes(List<BoxCluster> clusters);
}

public class AgreementService : IAgreementService
{
	/// <summary>
	/// Majority vote: for each cluster, score = annotators voting majority label / total annotators in that cluster.
	/// Overall score = average across all clusters.
	/// </summary>
	public double CalculateOverallScore(List<BoxCluster> clusters, int totalAnnotators)
	{
		if (clusters.Count == 0 || totalAnnotators <= 0)
			return 0;

		var clusterScores = clusters.Select(cluster =>
		{
			var majorityLabel = cluster.Members
				.GroupBy(m => m.Label)
				.OrderByDescending(g => g.Count())
				.First().Key;

			var majorityVotes = cluster.Members
				.Where(m => m.Label == majorityLabel)
				.Select(m => m.AnnotatorId)
				.Distinct()
				.Count();

			var totalVoters = cluster.Members
				.Select(m => m.AnnotatorId)
				.Distinct()
				.Count();

			return (double)majorityVotes / totalVoters;
		});

		return clusterScores.Average();
	}

	public List<ConsensusBboxDto> BuildConsensusBboxes(List<BoxCluster> clusters)
	{
		return clusters.Select(cluster =>
		{
			var labelGroup = cluster.Members
				.GroupBy(m => m.Label)
				.OrderByDescending(g => g.Count())
				.First();

			var majorLabelMembers = cluster.Members
				.Where(m => m.Label == labelGroup.Key)
				.ToList();

			return new ConsensusBboxDto
			{
				Label = labelGroup.Key,
				X = majorLabelMembers.Average(m => m.X),
				Y = majorLabelMembers.Average(m => m.Y),
				Width = majorLabelMembers.Average(m => m.Width),
				Height = majorLabelMembers.Average(m => m.Height),
				Support = majorLabelMembers.Select(m => m.AnnotatorId).Distinct().Count()
			};
		}).ToList();
	}
}
