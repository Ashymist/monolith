import "./Folder.css"

function Folder({name, currentPath, pointTo, updatePath}){
    return(
        <div className="folder" onClick={() => {updatePath(currentPath+pointTo)}}>
            <img src='/paper.png' className='folder_icon'></img>
            <div className='foldername'>{name}</div>
        </div>
    )
}

export default Folder