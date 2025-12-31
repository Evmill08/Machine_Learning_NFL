import React from 'react';
import { HomePage } from './pages/Home';
import { persistQueryClient } from "@tanstack/query-persist-client-core";
import { createAsyncStoragePersister } from '@tanstack/query-async-storage-persister'; 
import { queryClient } from "./services/queryClient";
import { QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter, Routes, Route, Link } from 'react-router-dom';
import { GameDetails } from './pages/GameDetails';

persistQueryClient({
  queryClient,
  persister: createAsyncStoragePersister({
    storage: window.localStorage,
  })
})

function App() {
  return (
    <BrowserRouter>
      <QueryClientProvider client={queryClient}>
        <Routes>
          <Route path="/" element={<HomePage/>}/>
          <Route path="/game/:eventId" element={<GameDetails/>}/>
        </Routes>
        
      </QueryClientProvider>
    </BrowserRouter>
    
  );
}

export default App;