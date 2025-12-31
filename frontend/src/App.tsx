import React from 'react';
import { HomePage } from './pages/Home';
import { persistQueryClient } from "@tanstack/query-persist-client-core";
import { createAsyncStoragePersister } from '@tanstack/query-async-storage-persister'; 
import { queryClient } from "./services/queryClient";
import { QueryClientProvider } from '@tanstack/react-query';

persistQueryClient({
  queryClient,
  persister: createAsyncStoragePersister({
    storage: window.localStorage,
  })
})

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <HomePage />
    </QueryClientProvider>
  );
}

export default App;