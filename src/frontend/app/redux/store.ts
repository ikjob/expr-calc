import { configureStore } from '@reduxjs/toolkit'
import { setupListeners } from '@reduxjs/toolkit/query'
import { activeUserStateSlice } from './stores/userStore'
import { backendApi } from '../api/backendApi'

export const store = configureStore({
  reducer: {
    [activeUserStateSlice.reducerPath]: activeUserStateSlice.reducer,
    [backendApi.reducerPath]: backendApi.reducer
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware().concat(backendApi.middleware),
})

setupListeners(store.dispatch)

// Infer the `RootState` and `AppDispatch` types from the store itself
export type RootState = ReturnType<typeof store.getState>
// Inferred type: {posts: PostsState, comments: CommentsState, users: UsersState}
export type AppDispatch = typeof store.dispatch