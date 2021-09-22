import React, { useState } from "react";
import "./App.css";

function App() {
  const [todos, setTodos] = useState([]);

  return (
    <div className="App">
      <header className="App-header">
        <p>Cosmos Todo App</p>
      </header>
    </div>
  );
}

export default App;
