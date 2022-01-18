using CloudantDb.Runner.Test;



var crud = new Crud(Secrets.Account, Secrets.Db, Secrets.Auth, Secrets.Design);
var animals = await crud.GetAnimalsAsync();

List<Animal> newAnimals = new();

if (animals is not null)
	newAnimals = await crud.UpdateAnimalsAsync(animals);

foreach (var a in newAnimals)
	Console.WriteLine(a._id);


Console.WriteLine("Done.");
Console.ReadKey();
