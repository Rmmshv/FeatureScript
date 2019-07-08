FeatureScript 1096;
import(path : "onshape/std/geometry.fs", version : "1096.0");

/***
 * Feature calls curvePoint and sketchPipe functions and 
 * creates a sketch base for a sweep.
***/

annotation { "Feature Type Name" : "Sketch" }
export const mySketch = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
    }
    {
         sketchPipe(context, id);
         curvePoint(context, id);   
    });       
function curvePoint(context is Context, id is Id)
/***
 * Using a curve point we create a plane.
 * On this plane we create two circles to set the inner and outer radius for our sweep.
 * Then we use circles as a face to extrude the sweep itself.
***/

{
       var curve_point is array = [];
       curve_point = append(curve_point, qCreatedBy(id + "sketch1", EntityType.EDGE));
       curve_point = append(curve_point, qNthElement(qCreatedBy(id + "sketch1", EntityType.VERTEX), 0));
       debug(context, qNthElement(qCreatedBy(id + "sketch1", EntityType.VERTEX), 0));
       cPlane(context, id + "cplane", { 
                "entities" : qUnion(curve_point),
                "cplaneType" : CPlaneType.CURVE_POINT, 
                "flipAlignment" : false, 
                "width" : 6.0 * inch, 
                "height" : 6.0 * inch 
        }); 
    var cplane = evPlane(context, {
                "face" : qCreatedBy(id + "cplane", EntityType.FACE)
        });
    var sk_circle = newSketchOnPlane(context, id + "sketch2", {
            "sketchPlane" : cplane});
            
    skCircle(sk_circle, "circle1", {
                "center" : vector(0, 0) * millimeter,
                "radius" : 75 * millimeter         
        });
    skCircle(sk_circle, "circle2", {
                "center" : vector(0, 0) * millimeter,
                "radius" : 74 * millimeter
        });
        skSolve(sk_circle);
        sweep(context, id + "sweep", {
            "profiles" : qSketchRegion(id + "sketch2", true),
            "path" : qCreatedBy(id + "sketch1", EntityType.EDGE),
        });
        extrude(context, id + "extrude1", {
                "entities" : qNthElement(qCreatedBy(id + "sweep", EntityType.FACE), 1),
                "endBound" : BoundingType.BLIND,
                "depth" : 50 * millimeter
        });
       // debug(context, qEntityFilter(cPoint, EntityType.EDGE));
        
}

function sketchPipe(context is Context, id is Id)
/***
 *Function creates a sketch base for the future pipe. 
***/

{
     
        var sketch0 = newSketch(context, id + "sketch0", {
                "sketchPlane" : qCreatedBy(makeId("Top"), EntityType.FACE)
        });
        var sketch1 = newSketch(context, id + "sketch1", {
                    "sketchPlane" : qCreatedBy(makeId("Top"), EntityType.FACE)
            });
        skLineSegment(sketch0, "line1", {
                "start" : vector(0, 0) * millimeter,
                "end" : vector(0, -50) * millimeter
        });
        skArc(sketch1, "arc1", {
                    "start" : vector(0, -50) * millimeter,
                    "mid" : vector(-45, -148.8) * millimeter,
                    "end" : vector(-60, -160) * millimeter
            });
        skSolve(sketch0);
        skSolve(sketch1);  
}

/***
 * This feature creates upper part of the pipe.
 * We create two big circles to set the inner and outer radius and extrude them.
 * Then we create and extrude a small circle that is going to be used for a circlular pattern later.
***/ 

annotation { "Feature Type Name" : "Upper Circle" }
export const myCir = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
    }
    {
        // create and extrude circles
        var sketch1 = newSketch(context, id + "sketch1", {
                "sketchPlane" : qCreatedBy(makeId("Front"), EntityType.FACE) // outer circle
        });
        var sketch2 = newSketch(context, id + "sketch2", {
                "sketchPlane" : qCreatedBy(makeId("Front"), EntityType.FACE) // inner circle
        });
        
        skCircle(sketch1, "circle1", {
                "center" : vector(0, 0) * millimeter,
                "radius" : 150 * millimeter         
        });
        skCircle(sketch2, "circle2", {
                "center" : vector(0, 0) * millimeter,
                "radius" : 75 * millimeter
        });
    skSolve(sketch1);
    skSolve(sketch2);
    var sketch0 = newSketch(context, id + "sketch0", {
                "sketchPlane" : qCreatedBy(makeId("Front"), EntityType.FACE)
        });
        skCircle(sketch0, "circle0", {
                "center" : vector(135, 0) * millimeter,
                "radius" : 10 * millimeter
        });
        skSolve(sketch0);
    // extrude outer cir
    extrude(context, id + "extrude1", {
            "entities" : qSketchRegion(id + "sketch1"),
            "endBound" : BoundingType.BLIND,
            "depth" : -20 * millimeter
    });
    // extrude inner cir
    extrude(context, id + "extrude2", {
            "entities" : qSketchRegion(id + "sketch2"),
            "endBound" : BoundingType.BLIND,
            "operationType" : NewBodyOperationType.REMOVE,
            "depth" :-20 * millimeter
    });
    extrude(context, id + "extrude0", {
            "entities" : qSketchRegion(id + "sketch0"),
            "endBound" : BoundingType.BLIND,
            "operationType" : NewBodyOperationType.REMOVE,
            "depth" : -20 * millimeter
        });
    });

/***
 * Feature asks user for a mirror plane and entities to mirror,
 * then performs a regular mirror feature's behavior.
***/

annotation { "Feature Type Name" : "Mirror" }
export const myMirror = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        annotation { "Name" : "My Query", "Filter" : EntityType.FACE, "MaxNumberOfPicks" : 1 }
        definition.myQuery is Query;
     
        annotation { "Name" : "Features to pattern","Filter" : EntityType.BODY, }
        definition.instanceFunction is Query;
    }
    {
        mirror(context, id + "mirror", {
        "patternType" : MirrorType.PART,
        "entities" : definition.instanceFunction,
        "mirrorPlane" : qEntityFilter(definition.myQuery, EntityType.FACE),
        "defaultScope" : true// <-- same as turning off "Merge with all" in the feauture dialog
    });
    });
    
/***
 * First we're making a sketch line that will be used as the axis for our circular pattern.
 * Then we're choosing entitie to pattern and the feature makes 6 copies of it.
***/

annotation { "Feature Type Name" : "Circular Pattern" }
export const myCirPat = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
       annotation { "Name" : "Features to pattern", "Filter" : EntityType.FACE, }
        definition.instanceFunction is Query;
    }
    {
        var line = newSketch(context, id + "line", {
                "sketchPlane" : qCreatedBy(makeId("Top"), EntityType.FACE)
        });
        skLineSegment(line, "line1", {
                "start" : vector(0, 0) * millimeter,
                "end" : vector(0, 20) * millimeter
        });
        skSolve(line);
        debug(context, definition.instanceFunction);
        circularPattern(context, id + "circularPattern_arcs", {
                "patternType" : PatternType.PART,
                "entities" : definition.instanceFunction,
                "axis" : qCreatedBy(id + "line",  EntityType.EDGE),
                "angle" : 360 * degree,
                "instanceCount" : 6,
                "equalSpace" : true
        });           
    });
    
