namespace CloudantDb.Models;

public interface ICloudantObj
{
	string? _id { get; set; }
	string? _rev { get; set; }
	string tbl { get; set; }

}

