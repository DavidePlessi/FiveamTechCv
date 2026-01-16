// Create Vector Index for Projects
CREATE VECTOR INDEX vector_index IF NOT EXISTS
FOR (n:BaseNode)
ON (n.Embedding)
OPTIONS {indexConfig: {
 `vector.dimensions`: 768,
 `vector.similarity_function`: 'cosine'
}}
