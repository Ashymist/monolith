import './Mainbody.css'

function Mainbody({children}){
    return(
        <div className="mainbody_grid">
            {children}
        </div>
    );
}

export default Mainbody