Analyzing and Visualizing Data with F#
======================================

This repository contains source code for a report "Analyzing and Visualizing Data with F#" (to be) publsihed by O'Reilly.

Running the code
----------------

To run the code, download the source code or clone the repository. Then you need to restore packages. We are using the [Paket package manager](http://fsprojects.github.io/Paket/) for package management. If you are inside Xamarin Studio, MonoDevelop or Visual Studio, you should be able to just open the solution and build it. Alternatively, you can invoke Paket directly (on Windows, drop the `mono` prefix and use backslash):

    mono paket/paket.bootstrapper.exe
    mono paket/paket.exe restore
