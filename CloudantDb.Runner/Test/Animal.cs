using CloudantDb.Models;

namespace CloudantDb.Runner.Test
{
	public class Animal : ICloudantObj
	{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public string _id { get; set; }
		public string _rev { get; set; }
		public string tbl { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		public double Min_weight { get; set; }
		public double Max_weight { get; set; }
		public double Min_length { get; set; }
		public double Max_length { get; set; }


		public string? Latin_name { get; set; }
		public string? Wiki_page { get; set; }
		public string? @Class { get; set; }
		public string? Diet { get; set; }

	}
}
