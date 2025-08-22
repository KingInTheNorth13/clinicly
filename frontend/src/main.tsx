import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import App from './App.tsx';
import { ThemeProvider } from './contexts/ThemeContext';
import './index.css';

// Force dark mode text color for debugging
const style = document.createElement('style');
style.innerHTML = `
  .dark body,
  .dark {
    color: white !important;
  }
`;
document.head.appendChild(style);


createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <ThemeProvider defaultTheme="dark" storageKey="vite-ui-theme">
      <App />
    </ThemeProvider>
  </StrictMode>
);
