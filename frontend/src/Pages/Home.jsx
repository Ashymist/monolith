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
import OptionsMenu from "../Components/OptionsMenu.jsx";
import RenameMenu from "../Components/RenameMenu.jsx";


function Home(){

    const [files, setFiles] = useState([]);
    const [currentPath, setPath] = useState("/storage/");
    const [contextMenu, setContextMenu] = useState({
        position: {x:0,y:0},
        toggled: false,
        file: ""
    });
    const [renameMenu, setRenameMenu] = useState({
        file: "",
        toggled : false
    });
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

    const updateFiles = async () => {
        const res = await fetch("http://localhost:5173/api/storage");
            const status = res.status;
            if(status == 401 ) navigateToLogin('/login');
            if(status == 200) {
                res.json().then(data => {
                    setFiles(data);
            });
        }
    }

    const handleContextMenu = (e, reference) => {
        e.preventDefault();
        setContextMenu({
            position:{x:e.pageX, y:e.pageY},
            toggled:true,
            file:reference
        })
    };

    const hideContextMenu = (e) => {
        e.preventDefault();
        setContextMenu({
            position:{x:0, y:0},
            toggled:false,
            file: ""
        })
    }

    const openRenameMenu = (reference) => {
        setRenameMenu({
            file:reference,
            toggled:true
        });
    }

    const closeRenameMenu = () => {
        setRenameMenu({
            file:"",
            toggled:false
        })
    }

    const{filesToRender, foldersToRender} = useMemo(() => {
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
        return {filesToRender, foldersToRender:[...foldersToRender]}
    },[files, currentPath]);

    return(
        <Mainbody clickHandler={hideContextMenu}>
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
                        contextMenuHandler={handleContextMenu}
                        key={filesToRender.reference}
                    />)
                })}
            </Filegrid>

            <OptionsMenu 
            isToggled={contextMenu.toggled} 
            positionX={contextMenu.position.x} 
            positionY={contextMenu.position.y} 
            file={contextMenu.file}
            openRenameMenu={openRenameMenu}
            />

            <RenameMenu isToggled={renameMenu.toggled} fileReference={renameMenu.file} closeRenameMenu={closeRenameMenu} updateFiles={updateFiles}/>
        </Mainbody>

        
    );
}

function renderAsFile(path, currentPath){
    const prefix = path.replace(currentPath, "").indexOf('/');
    if(prefix == -1) return true; else return false; 
}

export default Home