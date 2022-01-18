using UtilitiesMaster.ExtMethods.ExtBool;
using UtilitiesMaster.ExtMethods.ExtString;

namespace CloudantDb.Models
{
	public class QueryParams
	{
		//conflicts (boolean) – Includes conflicts information in response. Ignored if include_docs isn’t true. Default is false
		public bool? Conflicts { get; set; }

		//descending (boolean) – Return the documents in descending by key order. Default is false
		public bool? Descending { get; set; }

		//endkey (json) – Stop returning records when the specified key is reached. Optional
		public string? Endkey { get; set; }

		//endkey_docid (string) – Stop returning records when the specified document ID is reached. Optional
		public string? Endkey_Docid { get; set; }

		//group (boolean) – Group the results using the reduce function to a group or single row. Default is false
		public bool? Group { get; set; }

		//group_level (number) – Specify the group level to be used. Optional
		public int? Group_Level { get; set; }

		//include_docs (boolean) – Include the associated document with each row. Default is false.
		public bool? Include_Docs { get; set; }

		//attachments (boolean) – Include the Base64-encoded content of attachments
		//  in the documents that are included if include_docs is true. Ignored if include_docs isn’t true. Default is false.
		public bool? Attachments { get; set; }

		//att_encoding_info (boolean) – Include encoding information in attachment stubs
		//  if include_docs is true and the particular attachment is compressed. Ignored if include_docs isn’t true. Default is false.
		public bool? Att_Encoding_Info { get; set; }

		//inclusive_end (boolean) – Specifies whether the specified end key should be included in the result. Default is true
		public bool? Inclusive_End { get; set; }

		//key (json) – Return only documents that match the specified key. Optional
		public string? Key { get; set; }

		//key -- Added to handle an array key value ***
		public string[]? KeyAsArray { get; set; }

		//keys (json-array) – Return only documents where the key matches one of the keys specified in the array. Optional
		public string? Keys { get; set; }

		//limit (number) – Limit the number of the returned documents to the specified number. Optional
		public int? Limit { get; set; }

		//reduce (boolean) – Use the reduction function. Default is true
		public bool? Reduce { get; set; }

		//skip (number) – Skip this number of records before starting to return the results. Default is 0
		public int? Skip { get; set; }

		//stale (string) – Allow the results from a stale view to be used. Supported values: ok and update_after. Optional
		public string? Stale { get; set; }

		//startkey (json) – Return records starting with the specified key. Optional
		public string? Startkey { get; set; }

		//startkey_docid (string) – Return records starting with the specified document ID. Optional
		public string? Startkey_Docid { get; set; }

		//update_seq (boolean) – Response includes an update_seq value indicating which sequence id of the database the view reflects. Default is false
		public bool? Update_Seq { get; set; }


		public string RenderParamString()
		{
			var pl = new List<string>();

			if (this.Conflicts.HasValue)
				pl.Add(MakeParam("conflicts", this.Conflicts.AsTrueFalse()));

			if (this.Descending.HasValue)
				pl.Add(MakeParam("descending", this.Descending.AsTrueFalse()));

			//endkey (json) – Stop r
			if (this.Endkey.IsNotEmpty())
				pl.Add(MakeParam("endkey", @"""" + this.Endkey + @""""));

			//endkey_docid (string)
			if (this.Endkey_Docid.IsNotEmpty())
				pl.Add(MakeParam("endkey_docid", this.Endkey_Docid!));

			//group (boolean) – Group
			if (this.Group.HasValue)
				pl.Add(MakeParam("group", this.Group.AsTrueFalse()));

			//group_level (number) –
			if (this.Group_Level.HasValue)
				pl.Add(MakeParam("group_level", this.Group_Level.Value.ToString()));

			//include_docs (boolean)
			if (this.Include_Docs.HasValue)
				pl.Add(MakeParam("include_docs", this.Include_Docs.AsTrueFalse()));

			//attachments (boolean)
			if (this.Attachments.HasValue)
				pl.Add(MakeParam("attachments", this.Attachments.AsTrueFalse()));

			//att_encoding_info (boolean)
			if (this.Att_Encoding_Info.HasValue)
				pl.Add(MakeParam("att_encoding_info", this.Att_Encoding_Info.AsTrueFalse()));

			//inclusive_end (boolean)
			if (this.Inclusive_End.HasValue)
				pl.Add(MakeParam("inclusive_end", this.Inclusive_End.AsTrueFalse()));

			//key (json) – Return on
			if (this.Key.IsNotEmpty())
				pl.Add(MakeParam("key", @"""" + this.Key + @""""));

			//key as array
			if (KeyAsArray != null && KeyAsArray.Length > 0)
				pl.Add(MakeParam("key", "[" + String.Join(",", KeyAsArray.Select(a => @"""" + a + @"""")) + "]"));

			//keys (json-array) – Re
			if (this.Keys.IsNotEmpty())
				pl.Add(MakeParam("keys", this.Keys!));

			//limit (number) – Limit
			if (this.Limit.HasValue)
				pl.Add(MakeParam("limit", this.Limit.Value.ToString()));

			//reduce (boolean) – Use
			if (this.Reduce.HasValue)
				pl.Add(MakeParam("reduce", this.Reduce.AsTrueFalse()));

			//skip (number) – Skip t
			if (this.Skip.HasValue)
				pl.Add(MakeParam("skip", this.Skip.Value.ToString()));

			//stale (string) – Allow
			if (this.Stale.IsNotEmpty())
				pl.Add(MakeParam("stale", this.Stale!));

			//startkey (json) – Retu
			if (this.Startkey.IsNotEmpty())
				pl.Add(MakeParam("startkey", @"""" + this.Startkey + @""""));

			//startkey_docid (string
			if (this.Startkey_Docid.IsNotEmpty())
				pl.Add(MakeParam("startkey_docid", this.Startkey_Docid!));

			//update_seq (boolean) –
			if (this.Update_Seq.HasValue)
				pl.Add(MakeParam("update_seq", this.Update_Seq.AsTrueFalse()));


			return String.Join("&", pl);
		}

		private string MakeParam(string key, string value)
		{
			return key + "=" + System.Net.WebUtility.UrlEncode(value);
		}

	}
}
