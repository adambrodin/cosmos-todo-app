export async function fetchAllTodos() {
  const response = await fetch("api/todos");
  return await response.json();
}
