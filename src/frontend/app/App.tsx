import { Routes, Route } from 'react-router'
import Layout from './pages/layout'
import CalculationsPage from './pages/calculations'
import AboutPage from './pages/about'
import LoginPage from './pages/login'
import RequireUserNameWrapper from './components/contextual/requireUserNameWrapper'

function App() {
    return (
        <>
            <Routes>
                <Route path="login" element={<LoginPage />} />
                <Route element={<RequireUserNameWrapper />}>
                    <Route element={<Layout />}>
                        <Route index element={<CalculationsPage />} />
                        <Route path="about" element={<AboutPage />} />
                    </Route>
                </Route>
            </Routes>
        </>
    )
}

export default App
