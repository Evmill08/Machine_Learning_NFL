import React from 'react';
import './App.css';
import { MainPageExample } from "./MainPageExample";  
import { persistQueryClient } from "@tanstack/query-persist-client-core";
import { createAsyncStoragePersister } from '@tanstack/query-async-storage-persister'; 
import { queryClient } from "./services/queryClient";

persistQueryClient({
  queryClient,
  persister: createAsyncStoragePersister({
    storage: window.localStorage,
  })
})

function App() {
  return (
    <div className="App">
      <MainPageExample />
    </div>
  );
}

export default App;