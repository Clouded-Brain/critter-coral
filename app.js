const STORAGE_KEY = "critter-coral:list";

const form = document.getElementById("critter-form");
const list = document.getElementById("list");
const emptyState = document.getElementById("empty-state");
const clearBtn = document.getElementById("clear");

const escapeHtml = (value) =>
  value
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;")
    .replaceAll("'", "&#039;");

const loadCritters = () => {
  try {
    const parsed = JSON.parse(localStorage.getItem(STORAGE_KEY) ?? "[]");
    return Array.isArray(parsed) ? parsed : [];
  } catch {
    return [];
  }
};

let critters = loadCritters();

const save = () => localStorage.setItem(STORAGE_KEY, JSON.stringify(critters));

const render = () => {
  list.innerHTML = "";

  for (const critter of critters) {
    const li = document.createElement("li");
    li.className = "item";
    li.innerHTML = `
      <div>
        <strong>${escapeHtml(critter.name)}</strong>
        <div class="meta">${escapeHtml(critter.species)} · ${escapeHtml(critter.mood)}</div>
      </div>
      <button type="button" data-id="${critter.id}" aria-label="Remove ${escapeHtml(critter.name)}">Remove</button>
    `;
    list.append(li);
  }

  emptyState.hidden = critters.length !== 0;
  clearBtn.disabled = critters.length === 0;
};

form.addEventListener("submit", (event) => {
  event.preventDefault();
  const data = new FormData(form);

  critters.unshift({
    id: crypto.randomUUID(),
    name: String(data.get("name") ?? "").trim(),
    species: String(data.get("species") ?? "").trim(),
    mood: String(data.get("mood") ?? "Curious"),
  });

  critters = critters.filter((c) => c.name && c.species);
  save();
  render();
  form.reset();
  document.getElementById("name").focus();
});

list.addEventListener("click", (event) => {
  const target = event.target;
  if (!(target instanceof HTMLButtonElement)) {
    return;
  }

  const id = target.dataset.id;
  critters = critters.filter((critter) => critter.id !== id);
  save();
  render();
});

clearBtn.addEventListener("click", () => {
  critters = [];
  save();
  render();
});

render();
