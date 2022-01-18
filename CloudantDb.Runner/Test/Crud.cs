using CloudantDb.Models;
using CloudantDb.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudantDb.Runner.Test
{
	public class Crud
	{
		private string account;
		private string db;
		private string auth;
		private string design;

		public Crud(string account, string db, string auth, string design)
		{
			this.account = account;
			this.db = db;
			this.auth = auth;
			this.design = design;
		}


		public async Task<List<Animal>> GetAnimalsAsync()
		{
			using var svc = new DbService(account, db, auth);

			var qp = new QueryParams { Include_Docs = true };

			var res = await svc.GetViewItemsAsync<Animal>(design, "all-not-design", qp);
			return res;
		}

		public async Task<List<Animal>> UpdateAnimalsAsync(List<Animal> animals)
		{
			using var svc = new DbService(account, db, auth);
			var res = new List<Animal>();

			foreach (var a in animals)
			{
				a.tbl = "animal";
				res.Add(await svc.UpdateItemAsync(a));
				Thread.Sleep(500);
			}

			return res;
		}

	}
}
