# Use 302 redirects

General discussion on redirection for a URL shortner service.

## Types of redirects
HTTP redirection has 2 general "flavors" that use a 3XX http status code to indicate to browsers to redirect.

 301: Permanently moved - these redirects indicate that a page has permanently move to a new location. Browsers MAY cache
 this to directly go to the redirect URL on subsequent visits to the shortened URL to short-circuit an unnecessary round trip.
 
 This is not ideal for us since we are interested in tracking statistics about vists to our short URL.

302: Found - this is a temporary redirect. Browsers SHOULD NOT cache this redirect and should always go through the short URL
 to the long URL. This is better for us since we can track statistics about visits to our short URL.