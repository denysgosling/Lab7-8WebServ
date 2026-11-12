document.querySelectorAll('[data-counter]').forEach(input => {
  const output = document.getElementById(input.dataset.counter);
  const update = () => output.textContent = `${input.value.length} / ${input.maxLength}`;
  input.addEventListener('input', update); update();
});
const fileInput = document.querySelector('input[type="file"]');
if (fileInput) fileInput.addEventListener('change', () => {
  const file = fileInput.files[0];
  document.getElementById('file-hint').textContent = file ? `${file.name} · ${(file.size / 1024).toFixed(1)} KB` : fileInput.dataset.empty;
});
