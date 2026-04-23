import "./Folder.css"

function Folder({name, pointTo}){
    return(
        <div className="folder">
            <img src='/paper.png' className='folder_icon'></img>
            <div className='foldername'>{name}</div>
        </div>
    )
}

export default Folder