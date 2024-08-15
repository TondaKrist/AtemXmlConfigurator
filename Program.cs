using System.Text;
using System.Xml.Linq;
using AtemXmlConfigurator;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddRazorPages();


var app = builder.Build();

app.MapRazorPages();

XDocument doc = new XDocument();

app.MapPost("/uploadXml", async (HttpRequest request) =>
{
    var file = request.Form.Files[0];
    var tempFilePath = Path.GetTempFileName();
    await using (var stream = File.Create(tempFilePath))
    {
        await file.CopyToAsync(stream);
    }
    
    doc = XDocument.Load(tempFilePath);

    // Parse Audio Sources into a List<Source>
    List<Source> sources = doc.Descendants("Source")
        .Select(x => new Source
        {
            Id = x.Attribute("id")?.Value,
            Name = x.Attribute("name")?.Value
        }).ToList();

    // Parse Audio Outputs into a List<Output>
    List<Output> outputs = doc.Descendants("Output")
        .Select(x => new Output
        {
            Id = x.Attribute("id")?.Value,
            SourceId = x.Attribute("sourceId")?.Value,
            Name = x.Attribute("name")?.Value
        }).ToList();

    return Results.Ok(new
    {
        Outputs = outputs,
        Sources = sources
    });
});

app.MapPost("updateConfig", (List<Output> outputs) =>
{
    foreach (var output in outputs)
    {
        // Find the corresponding Output element in the XML document
        var outputElement = doc.Descendants("Output")
            .FirstOrDefault(x => x.Attribute("id")?.Value == output.Id);

        if (outputElement != null)
        {
            // Update the sourceId attribute of the Output element
            outputElement.SetAttributeValue("sourceId", output.SourceId);
        }
    }

    // Save the updated XML document
  //return doc to the client as a XML file
  
  //create text stream
  var bytes = Encoding.UTF8.GetBytes(doc.ToString());
  
  return Results.File(bytes, "application/xml", "config.xml");
   
});

    

app.Run();
