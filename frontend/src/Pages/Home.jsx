import Sidebar from "../Components/Sidebar";
import Mainbody from "../Components/Mainbody";
import Header from "../Components/Header";
import './Home.css'
import { useEffect } from "react";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import Filegrid from "../Components/Filegrid";
import File from "../Components/File.jsx"
import { v4 as uuidv4 } from 'uuid';


function Home(){

    const [files, setFiles] = useState([]);
    const navigateToLogin = useNavigate();

    useEffect(() => {
        const authorize = async () => {
            const res = await fetch("http://localhost:5173/api/storage");
            const status = res.status;
            if(status == 401 ) navigateToLogin('/login');
            if(status == 200) {
                res.json().then(data => {setFiles(data)})
            }
        }

        authorize();
    },[]);

    return(
        <Mainbody>
            <Header path='/storage/'></Header>
            <Sidebar></Sidebar>
            <Filegrid>
                {files.map(file => (
                    <File 
                        reference={file.reference}
                        type={file.type}
                        byteSize={file.byteSize}
                        lastUpdated={file.lastUpdated}
                        name={file.name}
                        key={file.reference}
                    />
                ))}
            </Filegrid>
        </Mainbody>
    );
}

export default Home