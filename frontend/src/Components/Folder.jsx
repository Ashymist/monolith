import "./Folder.css"

function Folder({name, currentPath, pointTo, updatePath}){
    return(
        <div className="folder" onDoubleClick={() => {updatePath(currentPath+pointTo)}}>
            <img src='/01_Folder_48x48.webp' className='folder_icon'></img>
            <div className='foldername'>{name}</div>
        </div>
    )
}

export default Folder