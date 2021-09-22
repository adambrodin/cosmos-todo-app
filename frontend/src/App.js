import React, { useEffect, useState } from "react";
import "./App.css";
import TodoForm from "./components/TodoForm";
import TodoList from "./components/TodoList";
import Typography from "@material-ui/core/Typography";

function App() {
  const [todos, setTodos] = useState([]);

  // When page is initially loaded
  useEffect(() => {
    fetch("api/todos")
      .then((response) => response.json())
      .then((data) => {
        setTodos(data);
      })
      .catch(console.error);
  }, []);

  // Called whenever a todo changes
  useEffect(() => {}, [todos]);

  function addTodo(todo) {
    setTodos([todo, ...todos]);
  }

  // Called when checkbox is clicked
  function toggleComplete(id) {
    var updatedTodos = todos.map((todo) => {
      if (todo.id === id) {
        return { ...todo, completed: !todo.completed };
      }

      return todo;
    });

    setTodos(updatedTodos);
  }

  function removeTodo(id) {
    // Keeps all todos except for the one with remove id
    var updatedTodos = todos.filter((todo) => todo.id !== id);
    setTodos(updatedTodos);
  }

  return (
    <div className="App">
      <Typography variant="h2" style={{ padding: 20 }}>
        Cosmos Todo App
      </Typography>
      <TodoForm addTodo={addTodo} />
      <TodoList
        todos={todos}
        toggleComplete={toggleComplete}
        removeTodo={removeTodo}
      />
    </div>
  );
}

export default App;
