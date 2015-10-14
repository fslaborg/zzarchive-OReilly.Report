// ========================================================
// Chapter 2: Analyzing Data Using F# and Deedle
// ========================================================

#r "System.Xml.Linq.dll"
#load "packages/FsLab/FsLab.fsx"
open Deedle
open FSharp.Data
open XPlot.GoogleCharts
open XPlot.GoogleCharts.Deedle


// --------------------------------------------------------
// Downloading Data Using an XML Provider
// --------------------------------------------------------

type WorldData = XmlProvider<"http://api.worldbank.org/countries/indicators/NY.GDP.PCAP.CD?date=2010:2010">

// Root URL for data requests for the World Bank
let indUrl = "http://api.worldbank.org/countries/indicators/"

/// Given a year and an indicator, returns the data for 
/// all countries. The function issues one reqest for 
/// at most 1000 items (which is enough to get data for
/// all countries in the world).
let getData year indicator =
  let query =
    [ ("per_page","1000"); 
      ("date",sprintf "%d:%d" year year) ]
  let data = Http.RequestString(indUrl + indicator, query)
  let xml = WorldData.Parse(data)
  let orNaN value = 
    defaultArg (Option.map float value) nan
  series [ for d in xml.Datas ->
             d.Country.Value, orNaN d.Value ]

// --------------------------------------------------------
// Visualizing CO2 Emissions Change
// --------------------------------------------------------

// Download CO2 emission data - we use the World Bank type
// provider to get the indicator code, but then call the
// 'getData' function to downloda data efficiently

let wb = WorldBankData.GetDataContext()
let inds = wb.Countries.World.Indicators
let code = inds.``CO2 emissions (kt)``.IndicatorCode

let co2000 = getData 2000 code
let co2010 = getData 2010 code

// Plot the data for years 2000 and 2010
Chart.Geo(co2000)
Chart.Geo(co2010)

// Calculate the change (as %) over the last 10 years
let change = (co2010 - co2000) / co2000 * 100.0

// Plot the change between 2000 and 2010 and
// configure chart options (colors) to make it nicer
let cs = "#77D53D;#D1C855;#E8A958;#EA4C41;#930700"
let vs = [| -10;0;100;200;1000 |]
let axis = ColorAxis(values=vs, colors=cs.Split(';'))

Chart.Geo(change)
|> Chart.WithOptions(Options(colorAxis=axis))


// --------------------------------------------------------
// Aligning and Summarizing Data with Frames
// --------------------------------------------------------

// Obtain data about a number of different indicators 
// and combine them in a single Deedle data frame
let codes =
 [ "CO2", inds.``CO2 emissions (metric tons per capita)``
   "Univ", inds.``School enrollment, tertiary (% gross)``
   "Life", inds.``Life expectancy at birth, total (years)``
   "Growth", inds.``GDP per capita growth (annual %)``
   "Pop", inds.``Population growth (annual %)``
   "GDP", inds.``GDP per capita (current US$)`` ]

let world = 
  frame [ for name, ind in codes -> 
            name, getData 2010 ind.IndicatorCode ]

// Summarizing Data Using the R Provider
// (the following 3 lines will only run if you have
// R installed, otherwise you can skip them)
open RProvider
open RProvider.graphics

R.plot(world)

// Normalizing the World Data Set
let lo = Stats.min world
let hi = Stats.max world
let avg = Stats.mean world

// Fill missing values with an average value for the 
// indicator (this is very primitive, but works OK for us)
let filled =
  world
  |> Frame.transpose
  |> Frame.fillMissingUsing (fun _ ind -> avg.[ind])

// Rescale the values in the frame to the 0 .. 1 range
let norm =
  (filled - lo) / (hi - lo)
  |> Frame.transpose

// Build a chart that shows relationship between the logarithm
// of GDP and life expectancy in the world
let gdp = log norm.["GDP"] |> Series.values
let life = norm.["Life"] |> Series.values

let options = 
  Options(pointSize=3, colors=[|"#3B8FCC"|],
    trendlines=[|Trendline(opacity=0.5,lineWidth=10)|],
    hAxis=Axis(title="Log of scaled GDP (per capita)"),
    vAxis=Axis(title="Life expectancy (scaled)"))

Chart.Scatter(Seq.zip gdp life)
|> Chart.WithOptions(options)

// --------------------------------------------------------
// Scaling to the Cloud with MBrace
// --------------------------------------------------------

// For an M-Brace based version of the clustering sample
// that works on Azure cluster, see the following demo:
//
//  - https://github.com/tpetricek/Talks/blob/master/2015/scalable-ml-ds-fsharp/code-done/world-scale.fsx
// 
// To run it, you'll need an Azure cluster. The easiest
// way to create one is to use Briks by ElastaCloud.
// For more information see the M-Brace web site.
//
//  - http://www.briskengine.com
//  - http://www.m-brace.net/
//
// There is also more information about the topic in 
// the slides from a Machine Learning + Data Science 
// conference:
//
// - http://tpetricek.github.io/Talks/2015/scalable-ml-ds-fsharp/redmond/
//