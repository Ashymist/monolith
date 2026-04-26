import './Filegrid.css'

function Filegrid({children, handleDrop}){
    return(
        <div className="file_grid" onDrop={(e) => handleDrop(e)} onDragOver={(e) => {e.preventDefault()}}>
                {children}
        </div>
    );
}

export default Filegrid