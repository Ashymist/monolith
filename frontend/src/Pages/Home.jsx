import Sidebar from "../Components/Sidebar";
import Mainbody from "../Components/Mainbody";
import Header from "../Components/Header";
import './Home.css'
import { useEffect, useMemo } from "react";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import Filegrid from "../Components/Filegrid";
import File from "../Components/File.jsx"
import { v4 as uuidv4 } from 'uuid';
import Folder from "../Components/Folder.jsx"


function Home(){

    const [files, setFiles] = useState([]);
    const [currentPath, setPath] = useState("/storage/");
    const navigateToLogin = useNavigate();

    useEffect(() => {
        const authorize = async () => {
            const res = await fetch("http://localhost:5173/api/storage");
            const status = res.status;
            if(status == 401 ) navigateToLogin('/login');
            if(status == 200) {
                res.json().then(data => {
                    setFiles(data);

                });
                
            }
        }
        authorize();
    },[]);

    const{filesToRender, foldersToRender} = useMemo(() => {
        console.log("useMemo is activated")
        console.log("currentpath is" + currentPath);
        const foldersToRender = new Set();
        const filesToRender = [];
        
        files.forEach(file => {
            if(file.reference.startsWith("/api"+currentPath)){
                const path = file.reference.replace("/api","");
                if(renderAsFile(path, currentPath)){
                    filesToRender.push(file);
                } else {
                    foldersToRender.add(path.replace(currentPath, "").split("/")[0]);
                }
            }
            
        })

        console.log(foldersToRender);
        console.log(filesToRender);
        return {filesToRender, foldersToRender:[...foldersToRender]}
    },[files, currentPath]);

    return(
        <Mainbody>
            <Header path={currentPath} updatePath={setPath}></Header>
            <Sidebar></Sidebar>
            <Filegrid>
                {foldersToRender.map(foldername => (
                    <Folder
                        name = {foldername}
                        pointTo = {foldername + "/"}
                        currentPath={currentPath}
                        updatePath={setPath}
                        key={currentPath + foldername + "/"}
                    />
                ))}
                

                {filesToRender.map(filesToRender => {
                    return(<File 
                        reference={filesToRender.reference}
                        type={filesToRender.type}
                        byteSize={filesToRender.byteSize}
                        lastUpdated={filesToRender.lastUpdated}
                        name={filesToRender.name}
                        key={filesToRender.reference}
                    />)
                })}
            </Filegrid>
        </Mainbody>
    );
}

function renderAsFile(path, currentPath){
    console.log(path);
    const prefix = path.replace(currentPath, "").indexOf('/');
    console.log(prefix);
    if(prefix == -1) return true; else return false; 
}

export default Home