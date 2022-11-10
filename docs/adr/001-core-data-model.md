# Define core data model for a Shortned URL

This defines the core logic of implementing the URL shortner, the data model and the database schema.

## Generating Short URLs

There are two choices I considered for generating the short URL slugs:
1. Hash a long URL to something shorter and represent that hashed value as the slug
  - Pros of this approach:
	- The slugs are unique and can be used to identify the original URL
	- There are many options for off-the-shelf hash algorithms that could be used here like MD5, SHA1, etc.
  - Cons of this approach:
    - A long URL may not be reused easily as it will cause collisions
	  - An additional collisioning handle may be implemented but will be inefficient as it will involve repeated checks in the database
    - Adding multi tenancy (i.e. multiple users) can be potentially complex, especially if we want to allow different users to
	  reuse the same long URL but have separate short URLs
2. Generate a unique ID and derive a short string when creating a short URL and generate the slug
  - Pros of this approach:
    - The slugs are not derived from the long URL but rather from a randomly generated identifier and then saved to the database
	  - This is more scalable as we do not need to check the database for collisions.
    - Adding multi tenancy is easier as we can simply add a user ID to the short URL record and use that to identify the user
  - Cons of this approach:
    - The slugs are not deterministic and cannot be derived from the long URL
	- Needs design of some sort of unique id generation
	- Care needs to be taken not to use database for id generation and generate it in the service instead
	  - This can cause a bottleneck when scaling out
	- Care needs to be taken not to use sequential ids
	  - This can potentially make it easy to figure out the next slug which can be a security concern
	
**Decision:** Approach 2 is more suited a distributed system and is the chosen approach here. A random 32 bit id makes sense here.

## Encoding short URLs to be human-readable
Since the generated short urls are meant to be shared, it makes sense for them to use a standard character set. An easy way to do this
encode the bytes of the generated slug as a base 62 string to the end user. This can be represented with the character set
`[0-9, a-z, A-Z]` which enables `62^n` unique slugs where `n` is the length of the slug.

Since the slug is a 32 bit number, we can represent it as a base 62 string of length 6. This gives us `~62^6` unique slugs.

## Data Model
The data model for the short URL is as follows:
- Id: a random 32 bit identifier generated when the short url record is created
- LongUrl: a valid URL that visiting this short URL will redirect to
- Slug: a string up to 6 characters long that represents the shortened URL