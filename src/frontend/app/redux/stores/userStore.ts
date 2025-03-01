import { createSlice } from '@reduxjs/toolkit'
import type { PayloadAction } from '@reduxjs/toolkit'
import type { RootState } from '../store'

interface UserState {
  userName: string | null
}

const initialState: UserState = {
  userName: null,
}

export const activeUserStateSlice = createSlice({
  name: 'activeUserState',
  initialState,
  reducers: {
    setActiveUserName: (state, action: PayloadAction<string>) => {
      state.userName = action.payload
    },
  },
})

export const { setActiveUserName } = activeUserStateSlice.actions

export const selectActiveUserName = (state: RootState) => state.activeUserState.userName

export default activeUserStateSlice.reducer