// ========================================================
// Chapter 1: Accessing Data with Type Providers
// ========================================================

// Loads all the libraries that are included in the FsLab package
// (this way we do not need to load them one by one)
#load "packages/FsLab/FsLab.fsx"

// --------------------------------------------------------
// Getting Data from the World Bank
// --------------------------------------------------------

// Open the F# Data namespace and initialize a connection to World Bank
open FSharp.Data
let wb = WorldBankData.GetDataContext()

// Explore some of the indicators available from the World Bank
wb.Countries.``Czech Republic``.CapitalCity
wb.Countries.``Czech Republic``.Indicators
  .``CO2 emissions (kt)``.[2010]


// --------------------------------------------------------
// Calling the Open Weather Map REST API
// --------------------------------------------------------

// The Open Weather Map API now requires a (quick and free)
// registration. To make the tutorials easier to run, I 
// include a sample API key in the source code, but please
// obtain your own API key for any serious usage!
// (Visit: http://openweathermap.org/appid)
type Weather = JsonProvider<"http://api.openweathermap.org/data/2.5/forecast/daily?units=metric&q=Prague&APPID=cb63a1cf33894de710a1e3a64f036a27">

// Print the weather forecast (type '.' after 'day' to
// see what other information is returned from the service)
let w = Weather.GetSample()
printfn "%s" w.City.Country
for day in w.List do
  printfn "%f" day.Temp.Max


let baseUrl = "http://api.openweathermap.org/data/2.5"
let forecastUrl = baseUrl + "/forecast/daily?APPID=cb63a1cf33894de710a1e3a64f036a27&units=metric&q="

/// Returns the maximal expected temperature for tomorrow
/// for a specified place in the world (typically a city)
let getTomorrowTemp place =
  let w = Weather.Load(forecastUrl + place)
  let tomorrow = Seq.head w.List
  tomorrow.Temp.Max

getTomorrowTemp "Prague"
getTomorrowTemp "Cambridge,UK"


// --------------------------------------------------------
// Plotting Temperatures Around the World
// --------------------------------------------------------

// Get temperatures in capital cities of all countries in the world
let worldTemps =
  [ for c in wb.Countries ->
      let place = c.CapitalCity + "," + c.Name
      printfn "Getting temperature in: %s" place
      c.Name, getTomorrowTemp place ]

// Plot tomorrow's temperatures on a map
open XPlot.GoogleCharts
Chart.Geo(worldTemps)

// Make the chart nicer by specifying various chart options
// (set the colors for different temperatures - you might
// need to change this when running the code during winter!)

let colors = [| "#80E000";"#E0C000";"#E07B00";"#E02800" |]
let values = [| 0;+15;+30;+45 |]
let axis = ColorAxis(values=values, colors=colors)

worldTemps
|> Chart.Geo
|> Chart.WithOptions(Options(colorAxis=axis))
|> Chart.WithLabel "Temp"