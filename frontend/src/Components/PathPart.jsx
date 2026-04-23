import './PathPart.css'

function PathPart({part, updatePath, currentPath}){
    return(
        <>
            <div className="part" onClick={() => {updatePath(currentPath.substring(0, currentPath.indexOf(part)+part.length+1))}} >{part}</div>
            /
        </>
        
    )
}

export default PathPart