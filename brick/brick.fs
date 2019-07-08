FeatureScript 1096;
import(path : "onshape/std/geometry.fs", version : "1096.0");

export enum BrickType
{
    annotation { "Name" : "Brick" }
    REGULAR
}
annotation { "Feature Type Name" : "Brick correct" }
export const brickFeature = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        annotation { "Name" : "Type" }
        definition.brick is BrickType;
        annotation { "Name" : "Rows" }
        isInteger(definition.rows, POSITIVE_COUNT_BOUNDS);
        if (definition.brick == BrickType.REGULAR)
        {
            annotation { "Name" : "Columns" }
            isInteger(definition.columns, POSITIVE_COUNT_BOUNDS);
        }
    }
    {
        var brickBase;
        var noses;
        var studs;
        var studText;
        var studInnerIndent;
        var innerSupportColumn;
        var innerSupport;

        var technicHorizontalHole1;
        var technicHorizontalHole2;
        var technicHorizontalHole3;
        var technicHorizontalHole4;
        var technicVerticalHole;
        var flatIndent;

        // Create a rectangular sketch for the base shape of the brick
        brickBase = newSketch(context, id + "brickBase", {
                    "sketchPlane" : qCreatedBy(makeId("Top"), EntityType.FACE)
                });
        skRectangle(brickBase, "rectangle1", {
                    "firstCorner" : vector(0 + definition.dx, (0.8 * definition.rows) - 0.02 + definition.dz) * centimeter,
                    "secondCorner" : vector((0.8 * definition.columns) - 0.02 + definition.dx, 0 + definition.dz) * centimeter
                });
        skSolve(brickBase);

        // Extrude the base
        var height = 0.96;
        
        extrude(context, id + "brickBaseExtrude", {
                    "entities" : qSketchRegion(id + "brickBase"),
                    "endBound" : BoundingType.BLIND,
                    "depth" : height * centimeter,
                    "defaultScope" : false,
                    "operationType" : NewBodyOperationType.NEW
                });

        if (definition.flat == false) // Not a flat brick, will have studs and interior indent + support
        {
            // Studs on top face of the extruded base
            studs = newSketch(context, id + "studs", {
                        "sketchPlane" : qNthElement(qCreatedBy(id + "brickBaseExtrude", EntityType.FACE), 2)
                    });
           
            // Inner support and stud indent from the top face of the extruded base
            studInnerIndent = newSketch(context, id + "studInnerIndent", {
                        "sketchPlane" : qNthElement(qCreatedBy(id + "brickBaseExtrude", EntityType.FACE), 2)
                    });
            innerSupport = newSketch(context, id + "innerSupport", {
                        "sketchPlane" : qNthElement(qCreatedBy(id + "brickBaseExtrude", EntityType.FACE), 2)
                    });
        }

        // Inner supports from the top face of the extruded base
        innerSupportColumn = newSketch(context, id + "innerSupportColumn", {
                    "sketchPlane" : qNthElement(qCreatedBy(id + "brickBaseExtrude", EntityType.FACE), 2)
                });
        var c;
        var r;
        var studCount = 0;
        var constraintMap = {};

        // Loop through all rows and columns adding studs, text, support and technic holes
        for (c = 0; c < definition.columns; c += 1)
        {
            for (r = 0; r < definition.rows; r += 1)
            {
                if (definition.flat == false)
                {
                    // Add a stud for every row and column
                    skCircle(studs, "circle1" ~ studCount, {
                                "center" : vector((0.39 + (c * 0.8)) + definition.dx, (0.39 + (r * 0.8)) + definition.dz) * centimeter,
                                "radius" : 0.245 * centimeter
                            });
                    // Inner indentation for studs
                    skCircle(studInnerIndent, "circle2" ~ studCount, {
                                "center" : vector((0.39 + (c * 0.8)) + definition.dx, (0.39 + (r * 0.8)) + definition.dz) * centimeter,
                                "radius" : 0.14 * centimeter
                            });
                }
                // Add interior supports when the brick size is at least 2x2
                if (definition.columns > 1 && definition.rows > 1)
                {
                    if ((r < definition.rows - 1) && (c < definition.columns - 1))
                    {
                        skCircle(innerSupportColumn, "circle3a" ~ studCount, {
                                    "center" : vector((0.78 + (c * 0.8)) + definition.dx + 0.01, (0.78 + (r * 0.8)) + 0.01 + definition.dz) * centimeter,
                                    "radius" : 6.45 * millimeter / 2
                                });
                        skCircle(innerSupportColumn, "circle3b" ~ studCount, {
                                    "center" : vector((0.78 + (c * 0.8)) + 0.01 + definition.dx, (0.78 + (r * 0.8)) + 0.01 + definition.dz) * centimeter,
                                    "radius" : 4.9 * millimeter / 2
                                });
                        // Add vertical holes between 2x2 studs on technic bricks
                        if (definition.shortTechnic)
                        {
                            skCircle(technicVerticalHole, "circle11" ~ studCount, {
                                        "center" : vector((0.78 + (c * 0.8)) + 0.01 + definition.dx, (0.78 + (r * 0.8)) + 0.01 + definition.dz) * centimeter,
                                        "radius" : 4.9 * millimeter / 2
                                    });
                        }
                    }
                }
                else
                {
                    // Varied interior supports when there is only one column
                    if ((definition.columns % 2 == 1) && (r < definition.rows - 1) && definition.rows > 1)
                    {
                        skCircle(innerSupportColumn, "circle3a" ~ studCount, {
                                    "center" : vector((0.39 + (c * 0.8)) + definition.dx, (0.78 + (r * 0.8)) + definition.dz) * centimeter,
                                    "radius" : 0.14 * centimeter
                                });
                        if (definition.short)
                        {
                            skCircle(innerSupportColumn, "circle3b" ~ studCount, {
                                        "center" : vector((0.39 + (c * 0.8)) + definition.dx, (0.78 + (r * 0.8)) + definition.dz) * centimeter,
                                        "radius" : 6.45 * millimeter / 2
                                    });
                        }
                        else
                        {
                            skRectangle(innerSupport, "rectangle3a" ~ studCount, {
                                        "firstCorner" : vector((c * 0.8) + definition.dx, (0.75 + (r * 0.8)) + definition.dz) * centimeter,
                                        "secondCorner" : vector((0.78 + (c * 0.8)) + definition.dx, (0.81 + (r * 0.8)) + definition.dz) * centimeter
                                    });
                        }
                    }
                    else if ((definition.rows % 2 == 1) && (c < definition.columns - 1) && definition.columns > 1)
                    {
                        skCircle(innerSupportColumn, "circle3a" ~ studCount, {
                                    "center" : vector((0.78 + (c * 0.8)) + definition.dx, (0.39 + (r * 0.8)) + definition.dz) * centimeter,
                                    "radius" : 0.14 * centimeter
                                });
                        if (definition.short)
                        {
                            skCircle(innerSupportColumn, "circle3b" ~ studCount, {
                                        "center" : vector((0.78 + (c * 0.8)) + definition.dx, (0.39 + (r * 0.8)) + definition.dz) * centimeter,
                                        "radius" : 6.45 * millimeter / 2
                                    });
                        }
                        else
                        {
                            skRectangle(innerSupport, "rectangle3a" ~ studCount, {
                                        "firstCorner" : vector((0.75 + (c * 0.8)) + definition.dx, (r * 0.8) + definition.dz) * centimeter,
                                        "secondCorner" : vector((0.81 + (c * 0.8)) + definition.dx, (0.78 + (r * 0.8)) + definition.dz) * centimeter
                                    });
                        }
                    }
                }
                studCount += 1;
            }
        }

        // Solve the built up sketches which include all the row, column dependent details
        if (definition.flat == false)
        {
            skSolve(studs);
            if (definition.text)
            {
                skSetInitialGuess(studText, constraintMap);
                skSolve(studText);
            }
            skSolve(studInnerIndent);
            skSolve(innerSupport);
        }
        skSolve(innerSupportColumn);

        // Solve sketches for technic holes
        if (definition.technic)
        {
            skSolve(technicHorizontalHole1);
            skSolve(technicHorizontalHole2);
            skSolve(technicHorizontalHole3);
            skSolve(technicHorizontalHole4);
        }
        if (definition.shortTechnic)
        {
            skSolve(technicVerticalHole);
        }

        // Shell the bottom face of the brick base to hollow it out
        shell(context, id + "shell1", {
                    "isHollow" : false,
                    "entities" : qUnion([qNthElement(qCreatedBy(id + "brickBaseExtrude", EntityType.FACE), 1)]),
                    "thickness" : 0.15 * centimeter
                });
          
        if (definition.flat == false)
        {
            // Extrude remove inner stud indent forbricks
            if (definition.technic != true)
            {
                extrude(context, id + "studInnerIndentExtrude", {
                            "entities" : qSketchRegion(id + "studInnerIndent"),
                            "endBound" : BoundingType.BLIND,
                            "operationType" : NewBodyOperationType.REMOVE,
                            "oppositeDirection" : true,
                            "depth" : 0.17 * centimeter,
                            "defaultScope" : false,
                            "booleanScope": qUnion([qCreatedBy(id + "brickBaseExtrude", EntityType.BODY)])
                        });
            }
            // Extrude the studs
            extrude(context, id + "studsExtrude", {
                        "entities" : qSketchRegion(id + "studs"),
                        "endBound" : BoundingType.BLIND,
                        "operationType" : NewBodyOperationType.ADD,
                        "depth" : 0.17 * centimeter,
                        "hasSecondDirection" : true,
                        "secondDirectionBound" : SecondDirectionBoundingType.BLIND,
                        "secondDirectionOppositeDirection" : true,
                        "secondDirectionBoundEntity" : qUnion([]),
                        "secondDirectionDepth" : 0.05 * centimeter,
                        "defaultScope" : false,
                        "booleanScope": qUnion([qCreatedBy(id + "brickBaseExtrude", EntityType.BODY)])
                    });

            // Extrude the supports when appropriate
            if (definition.short != true && definition.technic == false &&
                ((definition.rows == 1 && definition.columns > 1) ||
                        (definition.columns == 1 && definition.rows > 1)))
            {
                extrude(context, id + "innerSupportExtrude", {
                            "entities" : qSketchRegion(id + "innerSupport"),
                            "endBound" : BoundingType.BLIND,
                            "operationType" : NewBodyOperationType.ADD,
                            "oppositeDirection" : true,
                            "depth" : (height - 0.17) * centimeter,
                            "defaultScope" : false,
                            "booleanScope": qUnion([qCreatedBy(id + "brickBaseExtrude", EntityType.BODY)])
                        });
            }

            // Delete the solved sketches used so far
            deleteBodies(context, id + "studAndSupportDelete", {
                        "entities" : qUnion([qCreatedBy(id + "studs", EntityType.BODY),
                                             qCreatedBy(id + "studInnerIndent", EntityType.BODY),
                                             qCreatedBy(id + "innerSupport", EntityType.BODY)])
                    });
        }
        // Extrude the inner column supports when appropriate
        if (!(definition.rows == 1 && definition.columns == 1))
        {
            extrude(context, id + "innerSupportColumnExtrude", {
                        "entities" : qSketchRegion(id + "innerSupportColumn"),
                        "endBound" : BoundingType.BLIND,
                        "operationType" : NewBodyOperationType.ADD,
                        "oppositeDirection" : true,
                        "depth" : height * centimeter,
                        "defaultScope" : false,
                        "booleanScope": qUnion([qCreatedBy(id + "brickBaseExtrude", EntityType.BODY)])
                    });
        }
        
        deleteBodies(context, id + "baseAndSupportDelete", {
                    "entities" : qUnion([qCreatedBy(id + "innerSupportColumn", EntityType.BODY),
                                         qCreatedBy(id + "brickBase", EntityType.BODY)])
                });

        // Extrude horizontal holes for technic bricks
        if (definition.technic)
        {
            extrude(context, id + "technicHorizontalHoleExtrude1", {
                        "entities" : qSketchRegion(id + "technicHorizontalHole1"),
                        "endBound" : BoundingType.BLIND,
                        "operationType" : NewBodyOperationType.ADD,
                        "oppositeDirection" : true,
                        "depth" : 0.78 * centimeter,
                        "defaultScope" : false,
                        "booleanScope": qUnion([qCreatedBy(id + "brickBaseExtrude", EntityType.BODY)])
                    });
            extrude(context, id + "technicHorizontalHoleExtrude2", {
                        "entities" : qSketchRegion(id + "technicHorizontalHole2"),
                        "endBound" : BoundingType.BLIND,
                        "operationType" : NewBodyOperationType.REMOVE,
                        "oppositeDirection" : true,
                        "depth" : 0.78 * centimeter,
                        "defaultScope" : false,
                        "booleanScope": qUnion([qCreatedBy(id + "brickBaseExtrude", EntityType.BODY)])
                    });
            extrude(context, id + "technicHorizontalHoleExtrude3", {
                        "entities" : qSketchRegion(id + "technicHorizontalHole3"),
                        "endBound" : BoundingType.BLIND,
                        "operationType" : NewBodyOperationType.REMOVE,
                        "oppositeDirection" : true,
                        "depth" : 0.04 * centimeter,
                        "defaultScope" : false,
                        "booleanScope": qUnion([qCreatedBy(id + "brickBaseExtrude", EntityType.BODY)])
                    });
            extrude(context, id + "technicHorizontalHoleExtrude4", {
                        "entities" : qSketchRegion(id + "technicHorizontalHole4"),
                        "endBound" : BoundingType.BLIND,
                        "operationType" : NewBodyOperationType.REMOVE,
                        "oppositeDirection" : true,
                        "depth" : 0.04 * centimeter,
                        "defaultScope" : false,
                        "booleanScope": qUnion([qCreatedBy(id + "brickBaseExtrude", EntityType.BODY)])
                    });
            // Delete sketches used to build horizontal holes
            deleteBodies(context, id + "technicHorizontalDelete", {
                        "entities" : qUnion([qCreatedBy(id + "technicHorizontalHole1", EntityType.BODY),
                                             qCreatedBy(id + "technicHorizontalHole2", EntityType.BODY),
                                             qCreatedBy(id + "technicHorizontalHole3", EntityType.BODY),
                                             qCreatedBy(id + "technicHorizontalHole4", EntityType.BODY)])
                    });
        }
        
    }, { dx : 0, dz : 0, short : false, flat : false, shortTechnic : false, technic : false, text : false});
