import './App.css'
import Header from './Components/Header.jsx'
import Mainbody from './Components/Mainbody.jsx';
import Sidebar from './Components/Sidebar.jsx';
import Login from './Pages/Login.jsx'
import Home from './Pages/Home.jsx';

import { BrowserRouter, Routes, Route, Link, Navigate} from 'react-router-dom';

function App() {
    

    return(
        <BrowserRouter>
            <Routes>
                <Route path="/" element={<Navigate to="/home"/>}/>
                <Route path="/home" element={<Home/>}/>
                <Route path="/login" element={<Login/>}/>
            </Routes>
        </BrowserRouter>
    );
}

export default App
