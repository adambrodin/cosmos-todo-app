import React, { useState } from "react";
import { v4 as uuid } from "uuid";
import { TextField, Button } from "@material-ui/core";

function TodoForm({ addTodo }) {
  const [todo, setTodo] = useState({ id: "", name: "", completed: false });

  function handleTaskInputEvent(e) {
    setTodo({ ...todo, name: e.target.value });
  }

  function handleSubmit(e) {
    e.preventDefault();
    if (todo.name.trim()) {
      addTodo({ ...todo, id: uuid() });
      setTodo({ ...todo, name: "" });
    }
  }

  return (
    <form className="todo-input-form" onSubmit={handleSubmit}>
      <TextField
        label="Task name"
        type="text"
        name="task"
        value={todo.task}
        onChange={handleTaskInputEvent}
      ></TextField>
      <Button type="submit">Add Task</Button>
    </form>
  );
}

export default TodoForm;
