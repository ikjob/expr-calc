import { createSlice } from '@reduxjs/toolkit'
import type { PayloadAction } from '@reduxjs/toolkit'
import type { RootState } from '../store'

const USER_STORE_ACTIVE_USER_NAME = "exprCalcActiveUserName"

interface UserState {
    userName: string | null
}

const initialState: UserState = {
    userName: localStorage.getItem(USER_STORE_ACTIVE_USER_NAME),
}

export const activeUserStateSlice = createSlice({
    name: 'activeUserState',
    initialState,
    reducers: {
        setActiveUserName: (state, action: PayloadAction<string>) => {
            state.userName = action.payload;
            localStorage.setItem(USER_STORE_ACTIVE_USER_NAME, action.payload);
        },
    },
})

export const { setActiveUserName } = activeUserStateSlice.actions

export const selectActiveUserName = (state: RootState) => state.activeUserState.userName
export const iseActiveUserNameSet = (state: RootState) => !!state.activeUserState.userName

export default activeUserStateSlice.reducer