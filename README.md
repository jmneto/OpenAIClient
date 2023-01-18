# OpenAIClient
This GitHub project is an OpenAI client written in C#. It utilizes the OpenAI API to generate completions for a given prompt using the specified model and parameters.   

The user interface includes input fields for the user to enter their OpenAI API key, the model they want to use, the prompt they want to generate completions for, the maximum number of tokens they want the completion to have, and the temperature of the completion.  

The user can save the completion context and use it in future requests  Allowing the user to continue a conversation with the model, where the previous response is used as the context for the following prompt.   

It utilizes the System.Text.Json namespace, a high-performance, and low-allocating JSON library. This allows for efficient JSON data handling when sending the request to the OpenAI API and when parsing the response.  

In summary, this GitHub project is a simple yet powerful OpenAI client, allowing users to quickly generate completions for a given prompt and use the completion context in future requests. It is built on top of the .NET framework, making it easy for developers familiar with C# and the Windows ecosystem to use and understand.  