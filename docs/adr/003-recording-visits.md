# Record visits asynchronously

Since recording a visit and increment visit count should not affect the scalability or speed or the redirect, it is preferred
to do this operation asynchronously. Typically this would be done using sort sort of message broker like RabbitMQ in a production app.

In this implementation, a simplified version of this is implemented with a thread safe in-memory queue. When a redirection occurs,
a visit is added to the queue. A background service running on a separate thread will then dequeue the visit and record it in the
database. It will additionally log information about the user's IP address and user agent. This data could be useful for
more granular analytics in a production app.