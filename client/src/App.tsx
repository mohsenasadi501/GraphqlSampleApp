import React from "react";
import logo from "./logo.svg";
import "./App.css";
import gql from "graphql-tag";
import { useQuery } from "@apollo/react-hooks";

const GET_USERS = gql`
  query {
    users {
      nodes {
        bio
        id
        emailAddress
      }
    }
  }
`;
function App() {
  const { data } = useQuery(GET_USERS);

  return (
    <div className="App">
      {console.log(data)}
      <header className="App-header">
        <img src={logo} className="App-logo" alt="logo" />
        <p>
          Edit <code>src/App.tsx</code> and save to reload.
        </p>
        <a
          className="App-link"
          href="https://reactjs.org"
          target="_blank"
          rel="noopener noreferrer"
        >
          Learn React
        </a>
      </header>
    </div>
  );
}

export default App;
