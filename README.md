# Kestrel Spikes
This is an example of a simple .Net Core web service that posts messages to a Storage Queue, Azure Service Bus, Hangfire, and a Ping.  
The purpose of this web service was to determine which queueing solution provided the quickest response.  The StorageQueueTest folder contains a .Net Core console application that uses Quartz to schedule calling each endpoint on the service every 30 seconds.
I spaced out the start times of each service call so they shouldn't run at the exact same time.  To get the timings in the image I am posting a StatsD timer to a Grafana host.  The timing is the time it took for the console app to call client.PostAsync (see code).

## Findings
I noticed significant spike on all of the endpoints, even the Ping endpoint which simply returns "Hello World".
The tallest spikes are exactly 1.5 seconds and the smaller spikes are exactly 1 second.

The questions is: What is causing these spikes?

![Grafana Response Times](/graphana.png?raw=true "Grafana Response Times")
