<Query Kind="Program">
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <Namespace>Newtonsoft.Json</Namespace>
</Query>

/*

* Install IITC (https://iitc.me/desktop/)
* Go to https://ingress.com/intel
* Navigate to the location of interest, and wait for it to load all portals
* Paste the javascript below into the browser console, and run it
* Move the map to an adjacent location of intrest, wait until loading is complete, and run the script again
* Repeat until all portals of intrest have been extracted
* Clear the console-window, and run the script one last time
* Save the console-output to a file
* Use this LINQPad-script to generate the config-lines needed

var disco = disco ? disco : {};
Object.keys(portals).forEach(x => {
	var p = portals[x];
	if(p.options.ent[2][8]) {
		disco[p._latlng] = { name: p.options.ent[2][8], lat: p._latlng.lat, lng: p._latlng.lng };
	}
});
Object.keys(disco).forEach(x => console.log(disco[x]));
console.log(`There are currently ${Object.keys(disco).length} unique portals detected`);

*/

void Main()
{
	var console = File.ReadAllText(@"C:\temp\www.ingress.com-1533727682322.log");
	var config = JsonConvert.DeserializeObject<AppConfig>(File.ReadAllText(@"C:\temp\RaidPlannerBot\AppConfig.json"));
	
	foreach(Match match in Regex.Matches(console, @"{name: ""(?<name>.*?)"", lat: (?<lat>.*?), lng: (?<lng>.*?)}", RegexOptions.Multiline | RegexOptions.Singleline))
	{
		var name = match.Groups["name"].Value.Trim();
		var lat = match.Groups["lat"].Value;
		var lng = match.Groups["lng"].Value;

		// Only output stuff that isn't already in the config
		if (!config.Locations.Any(l => l.Gyms.Any(g => g.Name == name)) && !config.Locations.Any(l => l.Pokestops.Any(g => g.Name == name)))
		{
			$"{{ \"name\": \"{name}\", \"latitude\": {lat}, \"longitude\": {lng}, \"aliases\": [ ] }},".Dump();
		}
	}
	
	"Done".Dump();
}

public class AppConfig
{
	[JsonProperty("locations")]
	public List<Location> Locations { get; set; }
}

public class Location
{
	[JsonProperty("gyms")]
	public List<Poi> Gyms { get; set; }
	
	[JsonProperty("pokestops")]
	public List<Poi> Pokestops { get; set; }
}

public class Poi
{
	[JsonProperty("name")]
	public string Name { get; set; }
}