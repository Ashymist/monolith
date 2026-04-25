import './Mainbody.css'

function Mainbody({children, clickHandler}){
    return(
        <div className="mainbody_grid" onClick={clickHandler}>
            {children}
        </div>
    );
}

export default Mainbody