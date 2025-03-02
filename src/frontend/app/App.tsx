import { Routes, Route } from 'react-router'
import Layout from './pages/layout'
import CalculationsPage from './pages/calculations'
import AboutPage from './pages/about'

function App() {
    return (
        <>
            <Routes>
                <Route element={<Layout />}>
                    <Route index element={<CalculationsPage />} />
                    <Route path='about' element={<AboutPage />} />
                </Route>
            </Routes>
        </>
    )
}

export default App
