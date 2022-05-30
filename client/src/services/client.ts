import {
  ApolloClient,
  InMemoryCache,
  HttpLink,
  ApolloLink,
  concat,
} from "@apollo/client";

const httpLink = new HttpLink({ uri: "https://localhost:7169/graphql" });
const authMiddleware = new ApolloLink((operation, forward) => {
  // add the authorization to the headers
  const token = localStorage.getItem("token");
  operation.setContext({
    headers: {
      authorization: token ? `Bearer ${token}` : "",
    },
  });
  return forward(operation);
});
const cache = new InMemoryCache();
const client = new ApolloClient({
  link: concat(authMiddleware, httpLink),
  cache,
});

export default client;
