import {
  ApolloClient,
  InMemoryCache,
  HttpLink,
  ApolloLink,
} from "@apollo/client";
const http = new HttpLink({ uri: "https://localhost:7169/graphql" });
const link = ApolloLink.from([http]);
const cache = new InMemoryCache();
const client = new ApolloClient({
  link,
  cache,
});

export default client;
